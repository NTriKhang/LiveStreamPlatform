﻿namespace BackendNet.Models.Submodel
{
    public class CourseStudent : SubUser
    {
        public CourseStudent(string user_id, string user_name, string user_avatar) 
            : base(user_id, user_name, user_avatar) 
        {
            rate = 0;
        }
        public float rate { get; set; }
    }
}
