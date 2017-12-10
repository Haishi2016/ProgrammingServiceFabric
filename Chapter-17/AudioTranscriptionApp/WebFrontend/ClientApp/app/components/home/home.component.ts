import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { Router } from '@angular/router';

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
    public uploading = false;
    private mRouter: Router;

    constructor(http: Http, router: Router, @Inject('BASE_URL') baseUrl: string) {
        this.mRouter = router;
        this.mHttp = http;
        this.mBaseUrl = baseUrl;
        this.filesToHandle = [];      
    }
    
    public submitFileJob() {
        this.uploading = true;
        const formData = new FormData();

        for (let file of this.filesToHandle)
            formData.append(file.name, file);

        var xHttp = new XMLHttpRequest();
        xHttp.upload.addEventListener("progress", (e) => { if (e.lengthComputable) { this.uploadProgress = Math.round(e.loaded / e.total * 100); } }, false);
        xHttp.upload.onloadend = (e) => {
            this.uploadProgress = 100;
            this.uploading = false;
            this.mRouter.navigate([ '/job' ]);            
        };
        xHttp.open("POST", this.mBaseUrl + "api/Job/SubmitFileJob", true);
        xHttp.send(formData);
    }
    public submitURLJob() {
        alert(this.jobUrl);
    }
    public fileChangeEvent(fileInput: any) {
        this.filesToHandle = <Array<File>>fileInput.target.files;
    }
}
