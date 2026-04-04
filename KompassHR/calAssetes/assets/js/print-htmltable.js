function printData(tableId) {
    var divToPrint = document.getElementById(tableId);
    newWin = window.open("");
    newWin.document.write(divToPrint.outerHTML);
    newWin.print();
    newWin.close();
}