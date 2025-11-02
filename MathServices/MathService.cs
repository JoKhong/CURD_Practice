using MathServiceContracts;

namespace MathServices
{
    public class MathService : ISum
    {
        public int Add(int first, int second)
        {
            return first + second;
        }
    }
}
