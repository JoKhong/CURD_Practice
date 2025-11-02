using MathServiceContracts;

namespace MathServices
{
    public class MathServices : ISum
    {
        public int AddTwo(int first, int second)
        {
            return first + second;
        }
    }
}
