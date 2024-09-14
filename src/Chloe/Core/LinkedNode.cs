namespace Chloe
{
    public class LinkedNode<T>
    {
        public LinkedNode()
        {

        }

        public LinkedNode(T value)
        {
            this.Value = value;
        }

        public LinkedNode<T> Previous { get; set; }
        public LinkedNode<T> Next { get; set; }
        public T Value { get; set; }
    }
}
