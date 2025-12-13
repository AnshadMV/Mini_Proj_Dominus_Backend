using Dominus.Domain.Interfaces;
using Dominus.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Dominus.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<Dominus.Domain.Interfaces.IAuthService, Dominus.Application.Services.AuthService>();
            services.AddScoped<Dominus.Domain.Interfaces.IUserService, Dominus.Application.Services.UserService>();
            services.AddScoped<Dominus.Application.Services.IProductService, Dominus.Application.Services.ProductService>();
            
            return services;
        }
    }
}

