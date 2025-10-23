//const { Dropdown } = require("bootstrap");

//function OnUnworkedContractCountsDateCallback(s, e) {
//    UnworkedViewCallbackPanel.PerformCallback();
//}

//function cbp_begincallback(s, e) {

//    //the devexpress DateEdit uses Javascript date format when grabbing it via javascript.  Below converts it to a format parsable by C#.
//    //see:  https://stackoverflow.com/questions/6702705/how-to-convert-javascript-datetime-to-c-sharp-datetime#:~:text=Use%20the%20DateTime.,mm%3Ass"%2C%20CultureInfo.

//    var date = unworkedContractCountsDate.GetDate();
//    var day = date.getDate();
//    var month = date.getMonth() + 1;    // yields month (add one as '.getMonth()' is zero indexed)
//    var year = date.getFullYear();  // yields year
//    //var hour = date.getHours();     // yields hours 
//    //var minute = date.getMinutes(); // yields minutes
//    //var second = date.getSeconds(); // yields seconds

//    // After this construct a string with the above results as below
//    var time = month + "/" + day + "/" + year;// + " " + hour + ':' + minute + ':' + second; 
//    e.customArgs["date"] = time;
//}

var fileDate
var contract

function mainWorklistBeginCallback(s, e) {

    e.customArgs["fileDate"] = $("#fileDate").text();
    e.customArgs["contract"] = $("#contract").text();
    window.clearTimeout(timeout);
}


function HccDiagLostFocus(s, e) {
    var hccAddCount = 12;
    var hccRemCount = 4;
    var numHccAddActions = 0;
    var numHccRemActions = 0;
    var dummy = col1.GetItemByName("hccactiondummy");
    var hccAddItem = col1.GetItemByName("RptCat_HCC_Add_Count");
    var hccRemItem = col1.GetItemByName("RptCat_HCC_Remove_Count");

    for (let i = 1; i <= hccAddCount; i++) {
        var fieldname;
        if (i < 10) {
            fieldname = "HCC_ADD_DX0" + i;
        }
        else {
            fieldname = "HCC_ADD_DX" + i;
        }
        var textbox = ASPxClientControl.GetControlCollection().GetByName(fieldname);
        var fieldValue = textbox.GetValue();
                
        if (fieldValue !== null) {
            numHccAddActions++;
        }
        if (numHccAddActions > 0) {
            RptCat_HCC_Add_Count.SetText(numHccAddActions);
            dummy.SetVisible(false);
            hccAddItem.SetVisible(true);
        }
        else {
            //dummy.SetVisible(true);
            hccAddItem.SetVisible(false);
        }

    }
    for (let i = 1; i <= hccRemCount; i++) {
        var fieldname = "HCC_REM_DX0" + i;
        var textbox = ASPxClientControl.GetControlCollection().GetByName(fieldname);
        var fieldValue = textbox.GetValue();

        if (fieldValue !== null) {
            numHccRemActions++;
        }

        if (numHccRemActions > 0) {
            RptCat_HCC_Remove_Count.SetText(numHccRemActions);
            dummy.SetVisible(false);
            hccRemItem.SetVisible(true);
        }
        else {
            //dummy.SetVisible(true);
            hccRemItem.SetVisible(false);
        }
    }
    if (numHccAddActions === 0 && numHccRemActions === 0) {
        dummy.SetVisible(true);
        hccRemItem.SetVisible(false);
        hccAddItem.SetVisible(false);
    }
}

function HccRemDiagLostFocus(s, e) {
    var hccRemCount = 4;
    var numHccRemActions = 0;
    var dummy = col1.GetItemByName("hccactiondummy");
    var hccRemItem = col1.GetItemByName("RptCat_HCC_Remove_Count");

    for (let i = 1; i <= hccRemCount; i++) {
        var fieldname = "HCC_REM_DX0" + i;
        var textbox = ASPxClientControl.GetControlCollection().GetByName(fieldname);
        var fieldValue = textbox.GetValue();

        if (fieldValue !== null) {
            numHccRemActions++;
        }

        if (numHccRemActions > 0) {
            RptCat_HCC_Rem_txtbx.SetText(numHccRemActions);
            dummy.SetVisible(false);
            hccRemItem.SetVisible(true);
        }
        else {
            //dummy.SetVisible(true);
            hccRemItem.SetVisible(false);

        }

        if (hccRemCanReset && hccAddCanReset) {
            dummy.SetVisible(true);
        }
    }
}

function NonHccDiagLostFocus(s, e) {
    
    var fieldcount = 4;
    var anyactions = false;
    var dummy = col1.GetItemByName("nonhccactiondummy");
    var nonhccrptitem = col1.GetItemByName("RptCat_NonHCC_Nochange")

    for (let i = 1; i <= fieldcount; i++) {
        var fieldname = "NonHCC_REM_DX0" + i;

        var textbox = ASPxClientControl.GetControlCollection().GetByName(fieldname);
        var fieldValue = textbox.GetValue();
        if (fieldValue !== null) {
            anyactions = true;
        }
    }

    for (let i = 1; i <= fieldcount; i++) {
        var fieldname = "NonHCC_ADD_DX0" + i;
        var textbox = ASPxClientControl.GetControlCollection().GetByName(fieldname);
        var fieldValue = textbox.GetValue();
        if (fieldValue !== null) {
            anyactions = true;
        }
    }

    if (anyactions === true) {
        dummy.SetVisible(false);
        RptCat_NonHCC_Change.SetChecked(true);
        nonhccrptitem.SetVisible(true);
    }
    else {
        dummy.SetVisible(true);
        RptCat_NonHCC_Change.SetChecked(false);
        nonhccrptitem.SetVisible(false);
    }
}

function hcc_reason_value_changed(s, e) {
    handle_hcc_combo_changed(); 

}

function hcc_reason_combo_prerender(s, e) {

}

function handle_hcc_combo_changed() {
    var fieldname;
    var new_count = 0;
    var specificity_count = 0;
    for (let i = 1; i <= 12; i++) {
        if (i < 10) {
            fieldname = "HCC_ADD_DX0" + i + "_Reason";
        }
        else {
            fieldname = "HCC_ADD_DX" + i + "_Reason";
        }
        var combo = ASPxClientControl.GetControlCollection().GetByName(fieldname);
        var value = combo.GetValue();
        if (value !== null) {
            if (value == "New") {
                new_count++;
            }
            else {
                specificity_count++;
            }
        }
    }

    var new_count_field = ASPxClientControl.GetControlCollection().GetByName("RptCat_HCC_Add_New");
    var specificity_count_field = ASPxClientControl.GetControlCollection().GetByName("RptCat_HCC_Add_Specificity");
    new_count_field.SetValue(new_count);
    specificity_count_field.SetValue(specificity_count);
}

function hold_chckbx_change(s, e) {
    var checkbox = RptCat_Hold.GetValue();
    var panel = col1.GetItemByName("hold_panel");
    var hold = ASPxClientComboBox.GetControlCollection().GetByName("Hold_Reason");
    var date = ASPxClientComboBox.GetControlCollection().GetByName("AddtoIssueListDate");
    var issue_by = ASPxClientComboBox.GetControlCollection().GetByName("AddtoIssueListBy");
    if (checkbox === true) {
        panel.SetVisible(true);        
        hold.SetIsValid(false);
        date.SetIsValid(false);
        issue_by.SetIsValid(false);

        //AddtoIssueListDate.SetValue(new Date());
    }
    else {
        panel.SetVisible(false);
        hold.SetIsValid(true);
        date.SetIsValid(true);
        issue_by.SetIsValid(true);
    }
}
function IsNavinaReview_chckbx_change(s, e) {
    var checkbox = IsNavinaReview.GetValue();
    var panel = col1.GetItemByName("NavinaICDGroup");
    var icd10Code01 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code01");
    var icd10Code01ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code01ConditionsValidated");
    if (checkbox === true) {
        panel.SetVisible(true);
        icd10Code01.SetIsValid(false);
        icd10Code01ConditionsValidated.SetIsValid(false);
    }
    else {
        panel.SetVisible(false);
        icd10Code01.SetIsValid(true);
        icd10Code01ConditionsValidated.SetIsValid(true);
    }
}
function SetDefaultDate(s, e) {
    ReviewDate.SetValue(new Date());
}

function SetWorkListEntryStartTime(s, e) {
    var today = new Date();
    StartTime.SetValue(today.getHours() + ":" + today.getMinutes() + ":" + today.getSeconds());
}

function init_hcc_reason(s, e) {
    var name = s.name;
    var hcc_txt_name = name.split("_Reason");
    var value = ASPxClientControl.GetControlCollection().GetByName(hcc_txt_name[0]).GetValue();
    if (value !== null) {
        s.SetEnabled(true);
    }
}

function hcc_action_init(s, e) {
    var value = s.GetValue();
    var item;
    if (value > 0) {
        if (s.name.includes("Add")) {
            item = col1.GetItemByName("RptCat_HCC_Add_Count");
        }
        else {
            item = col1.GetItemByName("RptCat_HCC_Remove_Count");
        }
        var dummy = col1.GetItemByName("hccactiondummy");
        item.SetVisible(true);
        dummy.SetVisible(false);
    }
}




