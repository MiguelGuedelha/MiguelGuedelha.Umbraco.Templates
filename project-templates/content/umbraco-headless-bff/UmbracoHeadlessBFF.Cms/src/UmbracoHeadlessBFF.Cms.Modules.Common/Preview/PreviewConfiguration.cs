using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Preview;

public static class FeaturesConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddPreviewModule()
        {
            builder.Services.Configure<PreviewOptions>(
                builder.Configuration.GetSection(PreviewOptions.SectionName));

            var previewConfig = builder.Configuration.GetSection(PreviewOptions.SectionName)
                .Get<PreviewOptions>() ?? throw new ArgumentException("No preview section configured");

            var key = Encoding.UTF8.GetBytes(previewConfig.SecretKey);

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidAlgorithms = [ SecurityAlgorithms.HmacSha512 ]
                };
            });
            builder.Services.AddAuthorization();
        }
    }
}
