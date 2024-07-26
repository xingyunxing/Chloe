
namespace Chloe.Dameng
{
    public class DamengOptions : DbOptions
    {
        public DamengOptions()
        {
            this.MaxNumberOfParameters = 32767;
            this.MaxInItems = 2048;
        }
    }
}
