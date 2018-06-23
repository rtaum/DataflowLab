using System.Threading.Tasks;

namespace DataflowLab
{
    public interface IDataFlow<T>
    {
        void BuildPipeline();

        void Link();

        Task SendAsyn(T item);

        Task Complete();
    }
}