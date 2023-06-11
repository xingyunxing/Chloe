namespace Chloe
{
    public class LinkeNode<T>
    {
        public LinkeNode()
        {

        }

        public LinkeNode(T value)
        {
            this.Value = value;
        }

        public LinkeNode<T> Previous { get; set; }
        public LinkeNode<T> Next { get; set; }
        public T Value { get; set; }
    }
}
