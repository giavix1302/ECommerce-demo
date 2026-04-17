using Domain.Enums;
using FluentValidation;

namespace Application.Features.Promotion.Commands.CreatePromotion;

public class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Promotion name is required.")
            .MaximumLength(100).WithMessage("Promotion name cannot exceed 100 characters.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be a non-negative integer.");

        // ✅ StartDate < EndDate
        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date.");

        RuleFor(x => x.Conditions)
            .NotEmpty().WithMessage("Conditions are required.");

        RuleFor(x => x.Actions)
            .NotEmpty().WithMessage("Action is required.");

        RuleForEach(x => x.Conditions)
            .SetValidator(new CreatePromotionConditionDtoValidator());

        RuleForEach(x => x.Actions)
            .SetValidator(new CreatePromotionActionDtoValidator());

        // Business Rules
        RuleFor(x => x).Custom((command, context) =>
        {
            // Actions must have exactly 1 element
            if (command.Actions == null || command.Actions.Count() != 1)
            {
                context.AddFailure("Actions", "Exactly one action is required.");
                return;
            }

            var action = command.Actions.First();

            // ===============================
            // BUY_X_GET_Y
            // ===============================
            if (command.Type == PromotionType.BUY_X_GET_Y)
            {
                var hasProductOrCategory = command.Conditions.Any(c =>
                    c.ConditionType == PromotionConditionType.PRODUCT ||
                    c.ConditionType == PromotionConditionType.CATEGORY);

                var hasMinQuantity = command.Conditions.Any(c =>
                    c.ConditionType == PromotionConditionType.MIN_QUANTITY);

                if (!hasProductOrCategory)
                {
                    context.AddFailure("Conditions",
                        "BUY_X_GET_Y must have at least one PRODUCT or CATEGORY condition.");
                }

                if (!hasMinQuantity)
                {
                    context.AddFailure("Conditions",
                        "BUY_X_GET_Y must have MIN_QUANTITY condition.");
                }

                if (action.ActionType != PromotionActionType.GIVE_GIFT)
                {
                    context.AddFailure("Actions",
                        "BUY_X_GET_Y must use GIVE_GIFT action.");
                }
            }

            // ===============================
            // CUSTOM_DISCOUNT
            // ===============================
            if (command.Type == PromotionType.CUSTOM_DISCOUNT)
            {
                if (action.ActionType != PromotionActionType.PERCENTAGE_DISCOUNT &&
                    action.ActionType != PromotionActionType.FIXED_DISCOUNT)
                {
                    context.AddFailure("Actions",
                        "CUSTOM_DISCOUNT must use PERCENTAGE_DISCOUNT or FIXED_DISCOUNT.");
                }
            }
        });
    }
}

public class CreatePromotionConditionDtoValidator : AbstractValidator<CreatePromotionConditionDto>
{
    public CreatePromotionConditionDtoValidator()
    {
        RuleFor(x => x.ConditionType)
            .IsInEnum().WithMessage("Invalid condition type.");

        // PRODUCT and CATEGORY require TargetId
        RuleFor(x => x.TargetId)
            .NotNull().WithMessage("TargetId is required for PRODUCT or CATEGORY condition.")
            .When(x => x.ConditionType == PromotionConditionType.PRODUCT ||
                       x.ConditionType == PromotionConditionType.CATEGORY);

        // MIN_QUANTITY and MIN_ORDER_VALUE require Value
        RuleFor(x => x.Value)
            .NotNull().WithMessage("Value is required for MIN_QUANTITY or MIN_ORDER_VALUE condition.")
            .GreaterThan(0).WithMessage("Value must be greater than 0.")
            .When(x => x.ConditionType == PromotionConditionType.MIN_QUANTITY ||
                       x.ConditionType == PromotionConditionType.MIN_ORDER_VALUE);
    }
}

public class CreatePromotionActionDtoValidator : AbstractValidator<CreatePromotionActionDto>
{
    public CreatePromotionActionDtoValidator()
    {
        RuleFor(x => x.ActionType)
            .IsInEnum().WithMessage("Invalid action type.");

        // PERCENTAGE_DISCOUNT and FIXED_DISCOUNT require DiscountValue
        RuleFor(x => x.DiscountValue)
            .NotNull().WithMessage("DiscountValue is required.")
            .GreaterThan(0).WithMessage("DiscountValue must be greater than 0.")
            .When(x => x.ActionType == PromotionActionType.PERCENTAGE_DISCOUNT ||
                       x.ActionType == PromotionActionType.FIXED_DISCOUNT);

        // PERCENTAGE_DISCOUNT: value must be between 0 and 100
        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100).WithMessage("Percentage discount cannot exceed 100.")
            .When(x => x.ActionType == PromotionActionType.PERCENTAGE_DISCOUNT);

        // GIVE_GIFT requires GiftProductId and GiftQuantity
        RuleFor(x => x.GiftProductId)
            .NotNull().WithMessage("GiftProductId is required for GIVE_GIFT action.")
            .When(x => x.ActionType == PromotionActionType.GIVE_GIFT);

        RuleFor(x => x.GiftQuantity)
            .NotNull().WithMessage("GiftQuantity is required for GIVE_GIFT action.")
            .GreaterThan(0).WithMessage("GiftQuantity must be greater than 0.")
            .When(x => x.ActionType == PromotionActionType.GIVE_GIFT);
    }
}
