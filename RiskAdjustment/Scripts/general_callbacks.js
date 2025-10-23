function OnUnworkedContractCountsDateCallback(s, e) {
    UnworkedViewCallbackPanel.PerformCallback();
}

function cbp_begincallback(s, e) {

    //the devexpress DateEdit uses Javascript date format when grabbing it via javascript.  Below converts it to a format parsable by C#.
    //see:  https://stackoverflow.com/questions/6702705/how-to-convert-javascript-datetime-to-c-sharp-datetime#:~:text=Use%20the%20DateTime.,mm%3Ass"%2C%20CultureInfo.

    var date = unworkedContractCountsDate.GetDate();
    var day = date.getDate();
    var month = date.getMonth() + 1;    // yields month (add one as '.getMonth()' is zero indexed)
    var year = date.getFullYear();  // yields year
    //var hour = date.getHours();     // yields hours 
    //var minute = date.getMinutes(); // yields minutes
    //var second = date.getSeconds(); // yields seconds

    // After this construct a string with the above results as below
    var time = month + "/" + day + "/" + year;// + " " + hour + ':' + minute + ':' + second; 
    e.customArgs["date"] = time;
}