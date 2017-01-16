using TinyIoC;

namespace Logzio.DotNet.Core.Bootstrap
{
    public interface IBootstraper
    {
        void Bootstrap();
        T Resolve<T>() where T : class;
    }

    public class Bootstraper : IBootstraper
    {
        public void Bootstrap()
        {
            TinyIoCContainer.Current.AutoRegister();
        }

        public T Resolve<T>() where T : class
        {
            return TinyIoCContainer.Current.Resolve<T>();
        }
    }
}