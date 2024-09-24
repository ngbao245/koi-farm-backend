﻿using AutoMapper;
using Repository.Data.Entity;
using Repository.Model.Auth;
using Repository.Model.Blog;
using Repository.Model.Product;
using Repository.Model.ProductItem;
using Repository.Model.Review;
using Repository.Model.User;

namespace Repository.Helper.AutoMapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Mapping between User and authentication models
            CreateMap<User, SignInModel>().ReverseMap();
            CreateMap<User, SignUpModel>().ReverseMap();

            // Mapping between User and response models
            CreateMap<User, ResponseUserModel>().ReverseMap();

            CreateMap<User, RequestCreateUserModel>().ReverseMap();

            CreateMap<Product, RequestCreateProductModel>().ReverseMap();
            CreateMap<Product, ResponseProductModel>().ReverseMap();

            CreateMap<Blog, RequestCreateBlogModel>().ReverseMap();
            CreateMap<Blog, ResponseBlogModel>().ReverseMap();

            CreateMap<ProductItem, RequestCreateProductItemModel>().ReverseMap();
            CreateMap<ProductItem, ResponseProductItemModel>().ReverseMap();

            CreateMap<Review, RequestCreateReviewModel>().ReverseMap();
            CreateMap<Review, ResponseReviewModel>().ReverseMap();


            // You can also map specific fields in case the properties differ
            CreateMap<UserRefreshToken, ResponseTokenModel>()
                .ForMember(dest => dest.Token, opt => opt.MapFrom(src => src.JwtId))
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.RefreshToken))
                .ReverseMap();
        }
    }
}
