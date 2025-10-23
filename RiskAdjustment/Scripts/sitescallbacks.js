function OnBeginCallback(s, e) {
    var value = s.GetRowKey(s.GetFocusedRowIndex());
    e.customArgs["Id"] = value;

}