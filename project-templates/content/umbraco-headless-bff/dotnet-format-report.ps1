# Define excluded paths
$EXCLUDED_PATHS_ARRAY = @(
    '.\UmbracoHeadlessBFF.Cms\src\UmbracoHeadlessBFF.Cms.Web\umbraco\'
    '.\UmbracoHeadlessBFF.Cms\src\UmbracoHeadlessBFF.Cms.Web\Views\'
    '.\UmbracoHeadlessBFF.Cms\src\UmbracoHeadlessBFF.Cms.Web\wwwroot\'
    '.\UmbracoHeadlessBFF.Cms\src\UmbracoHeadlessBFF.Cms.Web\uSync\'
    '.\UmbracoHeadlessBFF.Cms\src\UmbracoHeadlessBFF.Cms.Modules.Common\Umbraco/'
)

# Convert to proper arguments
$EXCLUDED_PATHS = $EXCLUDED_PATHS_ARRAY | ForEach-Object { "--exclude", $_ }

dotnet restore
dotnet build --no-restore

dotnet format style . --severity info --no-restore --verify-no-changes --verbosity normal @EXCLUDED_PATHS
dotnet format analyzers . --severity info --no-restore --verify-no-changes --verbosity normal @EXCLUDED_PATHS
dotnet format whitespace . --no-restore --verify-no-changes --verbosity normal @EXCLUDED_PATHS
