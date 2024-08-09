namespace BackendNet.Setting
{
    /// <summary>
    /// Sometime only a model is not enough, have some additional field for handle condition
    /// </summary>
    public class ReturnModel
    {
        public int code { set; get; }
        public string message { set; get; }
        public object entity { set; get; }
        public ReturnModel(int code, string message, object entity)
        {
            this.code = code;
            this.message = message;
            this.entity = entity;
        }
    }
}
