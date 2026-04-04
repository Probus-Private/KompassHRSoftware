
//function generateCaptcha() {
//        var alpha = new Array('A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
//                              'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
//                              'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
//                              'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z');
//        var i;
//        for (i = 0; i < 4; i++) {
//            var a = alpha[Math.floor(Math.random() * alpha.length)];
//            var b = alpha[Math.floor(Math.random() * alpha.length)];
//            var c = alpha[Math.floor(Math.random() * alpha.length)];
//            var d = alpha[Math.floor(Math.random() * alpha.length)];
//        }
//        var code = a + '' + b + '' + '' + c + '' + d;
//        debugger;
//        document.getElementById("mainCaptcha").value = code
//        //document.getElementById("mainCaptcha").html(code);
//    }
//function CheckValidCaptcha() {
//    var string1 = removeSpaces(document.getElementById('mainCaptcha').value);
//    var string2 = removeSpaces(document.getElementById('txtInput').value);
//    if (string1 == string2) {
//        //$("#success").html("Form is validated successfully");
//        $("#error").html("");
//        return true;
//    }
//    else {
//        $("#success").html("");
//        $("#error").html("Please enter a valid captcha.");
//        return false;
//    }
//}
//function removeSpaces(string) {
//    return string.split(' ').join('');
//}



//This Modal PopUp Open in Partial View
function SendNotification(Id) {
    debugger;
    generateCaptcha();
    $('#SendNotificationModalPopUp').modal('show');
    //$.ajax({
    //    type: "Get",
    //    url: "GetContactDetails",
    //    //data: { OutDoorCompanyId: OutDoorCompanyId, EmployeeId: EmployeeId },
    //    dataType: "text",
    //    success: function (data) {
    //        debugger;
    //        console.log(data);
    //        //alert(JSON.stringify(data[0]));
    //        var datas = JSON.parse(data);
    //        var Getdata = datas.data[0];
    //        var ManagerName1 = Getdata[2].Value;
    //        var ReportingmanagerMail1 = Getdata[9].Value
    //        var ManagerName2 = Getdata[3].Value;
    //        var EmailId = Getdata[10].Value
    //        var HR = Getdata[4].Value;
    //        var email = Getdata[11].Value;
    //        $('#lblManager').html(ManagerName1);
    //        $('#lblEmailIdName').html(ReportingmanagerMail1);
    //        $('#lblManagerName2').html(ManagerName2);
    //        $('#lblEmailId').html(EmailId);
    //        $('#lblHR').html(HR);
    //        $('#lblHREmailId').html(email);
    //        $('#ModalPopup').modal('show');
    //    }
    //});
}



