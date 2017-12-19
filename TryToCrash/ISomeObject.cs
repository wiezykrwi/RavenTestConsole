namespace TryToCrash
{
    internal interface ISomeObject
    {
        
        int Idke { get; set; }
    }

    class SomeObject : ISomeObject
    {
        public int Idke { get; set; }
        public string FieldProperty { get; set; }
    }
}