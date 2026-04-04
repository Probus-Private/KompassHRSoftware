import { Filesystem, Directory } from '@capacitor/filesystem';



const blob = pdf.output('blob');
const reader = new FileReader();

reader.onloadend = function () {
    const base64data = reader.result.split(',')[1]; // remove "data:application/pdf;base64,"

    Filesystem.writeFile({
        path: 'Payslip.pdf',
        data: base64data,
        directory: Directory.Documents,
    })
    .then(() => {
        alert("PDF saved!");
    })
    .catch((error) => {
        console.error("Error saving PDF:", error);
    });
};

reader.readAsDataURL(blob);

