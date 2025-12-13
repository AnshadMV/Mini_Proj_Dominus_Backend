//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace Dominus.Infrastructure.AutoMapper
//{

//    public class GeneralMappingProfile : Profile
//    {
//        public GeneralMappingProfile()
//        {
//            // DTO → Entity (for Add/Update)
//            CreateMap<CreateProductDTO, Product>()
//                .ForMember(dest => dest.AvailableSizes,
//                           opt => opt.MapFrom(src => src.AvailableSizes.Select(s => new ProductSize { Size = s }).ToList()))
//                .ForMember(dest => dest.Images, opt => opt.Ignore()) // handled in service
//                .ForMember(dest => dest.InStock, opt => opt.Ignore()) // handled in service
//                .ForMember(dest => dest.IsActive, opt => opt.Ignore()); // handled in service

//            CreateMap<UpdateProductDTO, Product>()
//                .ForMember(dest => dest.AvailableSizes,
//                           opt => opt.MapFrom(src => src.AvailableSizes.Select(s => new ProductSize { Size = s }).ToList()))
//                .ForMember(dest => dest.Images, opt => opt.Ignore())
//                .ForMember(dest => dest.InStock, opt => opt.Ignore())
//                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

//            // Entity → DTO (for Get operations)
//            CreateMap<Product, ProductDTO>()
//                .ForMember(dest => dest.AvailableSizes,
//                           opt => opt.MapFrom(src => src.AvailableSizes.Select(s => s.Size)));
//        }
//    }
//}
