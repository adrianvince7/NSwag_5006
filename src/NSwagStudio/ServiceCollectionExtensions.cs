using Microsoft.Extensions.DependencyInjection;
using NSwagStudio.ViewModels;
using NSwagStudio.ViewModels.CodeGenerators;
using NSwagStudio.ViewModels.SwaggerGenerators;

namespace NSwagStudio;

public static class ServiceCollectionExtensions {
    public static void AddCommonServices(this IServiceCollection services) {
        // Register each view model
        services.AddTransient<SwaggerToTypeScriptClientGeneratorViewModel>();
        services.AddTransient<SwaggerToCSharpClientGeneratorViewModel>();
        services.AddTransient<SwaggerToCSharpControllerGeneratorViewModel>();
        services.AddTransient<DocumentViewModel>();
        services.AddTransient<AspNetCoreToSwaggerGeneratorViewModel>();
    }
}