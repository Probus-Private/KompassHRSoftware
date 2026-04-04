using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KompassHR.Areas.Setting.Models.Setting_PhotoGalleryMaster
{
    public class Setting_PhotoGalleryMaster
    {
        public double PhotoGalleryMasterId { get; set; }
        public string PhotoGalleryMasterId_Encrypted { get; set; }
        public string DocNo { get; set; }
        public DateTime DocDate { get; set; }
        public string PhotoGalleryTitle { get; set; }
        public string PhotoGalleryDescription { get; set; }
        public string FileType { get; set; }
        public string URLFile { get; set; }
        public double PhotoGalleryId { get; set; }

        public string PhotoGalleryAlbumName { get; set; }

        public HttpPostedFileWrapper ImageFile { get; set; }




    }
}