namespace CSharp8
{
    public struct ReadOnlyMembers
    {
        public int Count { get; set; }

        void MutateState()
        {
            Count++;
        }

        readonly int MutateStateInReadOnly()
        {
            //Count++;
            return Count;
        }
    }
}
