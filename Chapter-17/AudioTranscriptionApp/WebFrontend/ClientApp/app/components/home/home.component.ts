import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'home',
    templateUrl: './home.component.html'
})
export class HomeComponent {
    public jobUrl = "";
    public uploadProgress:number = 0;
    public filesToHandle: Array<File>;
    private mHttp: Http;
    private mBaseUrl: string;

    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.mHttp = http;
        this.mBaseUrl = baseUrl;
        this.filesToHandle = [];
    }
    
    public submitFileJob() {
        const formData = new FormData();

        for (let file of this.filesToHandle)
            formData.append(file.name, file);

        this.mHttp.post(this.mBaseUrl + 'api/Job/SubmitFileJob', formData).subscribe(event => {
            console.log('Files uploaded!');
        }, error => console.error(error));
    }
    public submitURLJob() {
        alert(this.jobUrl);
    }
    public fileChangeEvent(fileInput: any) {
        this.filesToHandle = <Array<File>>fileInput.target.files;
    }
}
