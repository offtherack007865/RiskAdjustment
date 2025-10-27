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
    var icd10Code02 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code02");
    var icd10Code02ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code02ConditionsValidated");
    var icd10Code03 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code03");
    var icd10Code03ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code03ConditionsValidated");
    var icd10Code04 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code04");
    var icd10Code04ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code04ConditionsValidated");
    var icd10Code05 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code05");
    var icd10Code05ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code05ConditionsValidated");
    var icd10Code06 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code06");
    var icd10Code06ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code06ConditionsValidated");
    var icd10Code07 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code07");
    var icd10Code07ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code07ConditionsValidated");
    var icd10Code08 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code08");
    var icd10Code08ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code08ConditionsValidated");
    var icd10Code09 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code09");
    var icd10Code09ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code09ConditionsValidated");
    var icd10Code10 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code10");
    var icd10Code10ConditionsValidated = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code10ConditionsValidated");
    if (checkbox === true) {
        panel.SetVisible(true);
        icd10Code01.SetIsValid(false);
        icd10Code01ConditionsValidated.SetIsValid(false);
        icd10Code02.SetIsValid(false);
        icd10Code02ConditionsValidated.SetIsValid(false);
        icd10Code03.SetIsValid(false);
        icd10Code03ConditionsValidated.SetIsValid(false);
        icd10Code04.SetIsValid(false);
        icd10Code04ConditionsValidated.SetIsValid(false);
        icd10Code05.SetIsValid(false);
        icd10Code05ConditionsValidated.SetIsValid(false);
        icd10Code06.SetIsValid(false);
        icd10Code06ConditionsValidated.SetIsValid(false);
        icd10Code07.SetIsValid(false);
        icd10Code07ConditionsValidated.SetIsValid(false);
        icd10Code08.SetIsValid(false);
        icd10Code08ConditionsValidated.SetIsValid(false);
        icd10Code09.SetIsValid(false);
        icd10Code09ConditionsValidated.SetIsValid(false);
        icd10Code10.SetIsValid(false);
        icd10Code10ConditionsValidated.SetIsValid(false);
    }
    else {
        panel.SetVisible(false);
        icd10Code01.SetValue("");
        icd10Code01ConditionsValidated.SetChecked(false);
        icd10Code02.SetValue("");
        icd10Code02ConditionsValidated.SetChecked(false);
        icd10Code03.SetValue("");
        icd10Code03ConditionsValidated.SetChecked(false);
        icd10Code04.SetValue("");
        icd10Code04ConditionsValidated.SetChecked(false);
        icd10Code05.SetValue("");
        icd10Code05ConditionsValidated.SetChecked(false);
        icd10Code06.SetValue("");
        icd10Code06ConditionsValidated.SetChecked(false);
        icd10Code07.SetValue("");
        icd10Code07ConditionsValidated.SetChecked(false);
        icd10Code08.SetValue("");
        icd10Code08ConditionsValidated.SetChecked(false);
        icd10Code09.SetValue("");
        icd10Code09ConditionsValidated.SetChecked(false);
        icd10Code10.SetValue("");
        icd10Code10ConditionsValidated.SetChecked(false);

        icd10Code01.SetIsValid(true);
        icd10Code01ConditionsValidated.SetIsValid(true);
        icd10Code02.SetIsValid(true);
        icd10Code02ConditionsValidated.SetIsValid(true);
        icd10Code03.SetIsValid(true);
        icd10Code03ConditionsValidated.SetIsValid(true);
        icd10Code04.SetIsValid(true);
        icd10Code04ConditionsValidated.SetIsValid(true);
        icd10Code05.SetIsValid(true);
        icd10Code05ConditionsValidated.SetIsValid(true);
        icd10Code06.SetIsValid(true);
        icd10Code06ConditionsValidated.SetIsValid(true);
        icd10Code07.SetIsValid(true);
        icd10Code07ConditionsValidated.SetIsValid(true);
        icd10Code08.SetIsValid(true);
        icd10Code08ConditionsValidated.SetIsValid(true);
        icd10Code09.SetIsValid(true);
        icd10Code09ConditionsValidated.SetIsValid(true);
        icd10Code10.SetIsValid(true);
        icd10Code10ConditionsValidated.SetIsValid(true);
    }
}
function NavinaIcd10CodeLengthLe7(s, e) {
    var value = s.GetValue();
    if (value !== null && value.length > 7) {
        value = value.substring(0, 7);
        s.SetValue(value);
    }
    ValidateNavina(s, e);
}
function ValidateNavina(s, e) {
    var checkbox = IsNavinaReview.GetValue();
    var panel = col1.GetItemByName("NavinaICDGroup");

    var icd10Code01 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code01");
    var icd10Code02 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code02");
    var icd10Code03 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code03");
    var icd10Code04 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code04");
    var icd10Code05 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code05");
    var icd10Code06 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code06");
    var icd10Code07 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code07");
    var icd10Code08 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code08");
    var icd10Code09 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code09");
    var icd10Code10 = ASPxClientControl.GetControlCollection().GetByName("NavinaICD10Code10");

    if (checkbox === true) {
    
        var icd10Code01Value = icd10Code01.GetValue();
        var icd10Code02Value = icd10Code02.GetValue();
        var icd10Code03Value = icd10Code03.GetValue();
        var icd10Code04Value = icd10Code04.GetValue();
        var icd10Code05Value = icd10Code05.GetValue();
        var icd10Code06Value = icd10Code06.GetValue();
        var icd10Code07Value = icd10Code07.GetValue();
        var icd10Code08Value = icd10Code08.GetValue();
        var icd10Code09Value = icd10Code09.GetValue();
        var icd10Code10Value = icd10Code10.GetValue();

        if ((icd10Code01Value === null || icd10Code01Value.length === 0) &&
            (icd10Code02Value === null || icd10Code02Value.length === 0) &&
            (icd10Code03Value === null || icd10Code03Value.length === 0) &&
            (icd10Code04Value === null || icd10Code04Value.length === 0) &&
            (icd10Code05Value === null || icd10Code05Value.length === 0) &&
            (icd10Code06Value === null || icd10Code06Value.length === 0) &&
            (icd10Code07Value === null || icd10Code07Value.length === 0) &&
            (icd10Code08Value === null || icd10Code08Value.length === 0) &&
            (icd10Code09Value === null || icd10Code09Value.length === 0) &&
            (icd10Code10Value === null || icd10Code10Value.length === 0)) {

            IsNavinaReview.SetChecked(false);
            panel.SetVisible(false);    
        } 
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




