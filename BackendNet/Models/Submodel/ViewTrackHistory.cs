namespace BackendNet.Models.Submodel
{
    public class ViewTrackHistory
    {
        /// <summary>
        /// thời gian bắt đầu của 1 'Time Windows' 
        /// </summary>
        public DateTime timeStamp { get; set; }
        /// <summary>
        /// lượng views trong 1 'Time Windows'
        /// </summary>
        public int views { set; get; }
    }
}
