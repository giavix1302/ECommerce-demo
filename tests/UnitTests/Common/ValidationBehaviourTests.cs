using Application.Common.Behaviours;
using Application.Features.Coupon.Commands.CreateCoupon;
using Domain.Enums;
using FluentAssertions;
using FluentValidation;
using MediatR;
using NSubstitute;

namespace UnitTests.Common;

public class ValidationBehaviourTests
{
    private static ValidationBehaviour<CreateCouponCommand, CreateCouponResult> CreateBehaviour(
        IEnumerable<IValidator<CreateCouponCommand>> validators)
        => new(validators);

    private static CreateCouponCommand ValidCommand() => new(
        Code: "VALID10",
        DiscountType: DiscountType.FIXED_AMOUNT,
        Value: 50_000,
        MinOrderValue: 0,
        StartDate: DateTime.UtcNow,
        EndDate: DateTime.UtcNow.AddDays(7),
        UsageLimit: null
    );

    // MediatR 14: RequestHandlerDelegate<T> = Func<CancellationToken, Task<T>>
    private static RequestHandlerDelegate<CreateCouponResult> NextReturning(CreateCouponResult result)
        => _ => Task.FromResult(result);

    // ── No validators → pass through ─────────────────────────────────────────

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behaviour = CreateBehaviour([]);
        var nextCalled = false;
        RequestHandlerDelegate<CreateCouponResult> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(new CreateCouponResult(1, "VALID10"));
        };

        await behaviour.Handle(ValidCommand(), next, default);

        nextCalled.Should().BeTrue();
    }

    // ── Valid request → pass through ─────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validator = new CreateCouponCommandValidator();
        var behaviour = CreateBehaviour([validator]);
        var nextCalled = false;
        RequestHandlerDelegate<CreateCouponResult> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(new CreateCouponResult(1, "VALID10"));
        };

        await behaviour.Handle(ValidCommand(), next, default);

        nextCalled.Should().BeTrue();
    }

    // ── Invalid request → throws ValidationException ──────────────────────────

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var validator = new CreateCouponCommandValidator();
        var behaviour = CreateBehaviour([validator]);

        var invalidCmd = ValidCommand() with { Code = "" }; // empty Code fails

        var act = () => behaviour.Handle(invalidCmd, NextReturning(new CreateCouponResult(0, "")), default);

        await act.Should().ThrowAsync<Application.Common.Exceptions.ValidationException>();
    }

    // ── Multiple errors collected ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_MultipleFailures_ErrorsDictionaryContainsAllFields()
    {
        var validator = new CreateCouponCommandValidator();
        var behaviour = CreateBehaviour([validator]);

        var invalidCmd = ValidCommand() with
        {
            Code = "",       // fails NotEmpty
            Value = 0,       // fails GreaterThan(0)
            EndDate = DateTime.UtcNow.AddDays(-1) // fails > StartDate
        };

        Application.Common.Exceptions.ValidationException? ex = null;
        try
        {
            await behaviour.Handle(invalidCmd, NextReturning(new CreateCouponResult(0, "")), default);
        }
        catch (Application.Common.Exceptions.ValidationException e)
        {
            ex = e;
        }

        ex.Should().NotBeNull();
        ex!.Errors.Should().ContainKey("Code");
        ex.Errors.Should().ContainKey("Value");
    }

    // ── Next is NOT called on validation failure ───────────────────────────────

    [Fact]
    public async Task Handle_ValidationFails_NextIsNotCalled()
    {
        var validator = new CreateCouponCommandValidator();
        var behaviour = CreateBehaviour([validator]);
        var nextCalled = false;

        var invalidCmd = ValidCommand() with { Code = "" };

        try
        {
            await behaviour.Handle(invalidCmd, _ =>
            {
                nextCalled = true;
                return Task.FromResult(new CreateCouponResult(0, ""));
            }, default);
        }
        catch (Application.Common.Exceptions.ValidationException) { }

        nextCalled.Should().BeFalse();
    }

    // ── Multiple validators: all run ──────────────────────────────────────────

    [Fact]
    public async Task Handle_MultipleValidators_AllAreExecuted()
    {
        var v1 = Substitute.For<IValidator<CreateCouponCommand>>();
        var v2 = Substitute.For<IValidator<CreateCouponCommand>>();

        v1.Validate(Arg.Any<ValidationContext<CreateCouponCommand>>())
            .Returns(new FluentValidation.Results.ValidationResult());
        v2.Validate(Arg.Any<ValidationContext<CreateCouponCommand>>())
            .Returns(new FluentValidation.Results.ValidationResult());

        var behaviour = CreateBehaviour([v1, v2]);

        await behaviour.Handle(ValidCommand(), NextReturning(new CreateCouponResult(1, "X")), default);

        v1.Received(1).Validate(Arg.Any<ValidationContext<CreateCouponCommand>>());
        v2.Received(1).Validate(Arg.Any<ValidationContext<CreateCouponCommand>>());
    }
}
