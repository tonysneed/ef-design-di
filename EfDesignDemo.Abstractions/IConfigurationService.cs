using Microsoft.Extensions.Configuration;

namespace EfDesignDemo.Abstractions
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}