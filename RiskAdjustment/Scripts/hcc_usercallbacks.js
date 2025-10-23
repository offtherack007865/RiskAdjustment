function OnBeginCallback(s, e) {
    var value = s.GetRowKey(s.GetFocusedRowIndex());
    e.customArgs["Id"] = value;
}

var userExists; 
function ValidateUniqueEmail(s, e) {
    $.ajaxSetup({ async: false });
    $.get("/User/ValidateUniqueEmail",
        { email: e.value },
        function(data) {
            if (data === true) {
                userExists = true;
            } else {
                userExists = false;
            }
        });
    if (userExists === true) {
        e.isValid = false;
        e.errorText = "User already exists";
    }
    else{
        e.isValid = true;
    }
        
   
}

