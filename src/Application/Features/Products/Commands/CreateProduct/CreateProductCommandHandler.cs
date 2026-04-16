using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Variants = request.Variants.Select(v => new ProductVariant
            {
                Sku = v.Sku,
                Price = v.Price,
                StockQuantity = v.StockQuantity,
                IsDefault = v.IsDefault,
                AttributeValues = v.Attributes.Select(a => new VariantAttributeValue
                {
                    AttributeId = a.AttributeId,
                    Value = a.Value
                }).ToList()
            }).ToList()
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new CreateProductResult(product.Id, product.Name);
    }
}
