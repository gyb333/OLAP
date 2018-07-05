namespace Wilmar.SSAS.MDXBuilder.Errors
{
    using System;

    /// <summary>
    /// MDX异常
    /// </summary>
    public class MDXException : Exception
    {

        public MDXException() : base()
        {
        }

        public MDXException(string message) : base(message)
        {

        }
        public MDXException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        static public MDXException WhenBaseNotInit(object clazz)
        {
            return new MDXException($"{clazz.GetType().ToString()}未初始化");
        }

    }
}
