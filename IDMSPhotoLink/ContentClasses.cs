/*
 * Copyright ObjEx, Inc. ©  2014, All Rights Reserved
 * Licensed to Recipients under GNU GPL 3.0
 * 
 * ObjEx, Inc. is an Arizona Corporation and can be reached at
 * www.obj-ex.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDMSPhotoLink
{

    public class EmployeeViewedPostRequest
    {
        public string UserID { get; set; }
        public string Passphrase { get; set; }   // not yet implemented but strongly suggested
        public int ViewedEmployeeID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }

    public class EmployeeViewedPostResponse
    {
        public DateTime PostTime;
        public string status { get; set; }
        public string message { get; set; }
    }

    public class EmployeeViewedGetResponse
    {
        public string status { get; set; }
        public DateTime ReplyTime { get; set; }
        public string UserID { get; set; }
        public int EmployeeID { get; set; }
        public string PictureURL { get; set; }
        public string firstName { get; set; }
        /*
 * Copyright ObjEx, Inc. ©  2014, All Rights Reserved
 * Licensed to Recipients under GNU GPL 3.0
 * 
 * ObjEx, Inc. is an Arizona Corporation and can be reached at
 * www.obj-ex.com
 * 
 */
public string lastName { get; set; }
        public string VideoUri { get; set; }  // not used but left here to seed ideas
        public string message { get; set; }
    }


}