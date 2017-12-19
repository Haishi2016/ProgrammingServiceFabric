import { Component, Inject, OnInit } from '@angular/core';
import { Http } from '@angular/http';
import { Router } from '@angular/router';


@Component({
    selector: 'home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
    public jobUrl = "";
    public uploadProgress:number = 0;
    public filesToHandle: Array<File>;
    private mHttp: Http;
    private mBaseUrl: string;
    public uploading = false;
    private mRouter: Router;
    public hasFile: Boolean;
    public jobs: Job[];
    public userName = "";
    //private mTimer: any;

    constructor(http: Http, router: Router, @Inject('BASE_URL') baseUrl: string) {
        this.mRouter = router;
        this.mHttp = http;
        this.mBaseUrl = baseUrl;
        this.filesToHandle = [];      
        this.hasFile = false;
        //this.mTimer = setTimeout(() => { this.refreshPage() }, 0);
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
            this.hasFile = false;
            //this.mRouter.navigate([ '/job' ]);            
            //this.refreshPage();
        };
        xHttp.open("POST", this.mBaseUrl + "api/Job/SubmitFileJob", true);
        xHttp.send(formData);
    }
    public submitURLJob() {
        alert(this.jobUrl);
    }
    public fileChangeEvent(fileInput: any) {
        this.hasFile = true;
        this.filesToHandle = <Array<File>>fileInput.target.files;
        this.submitFileJob();
    }
    public refreshPage() {
        this.mHttp.get(this.mBaseUrl + 'api/Job/GetUserName').subscribe(result => {
            if (result.text())
                this.userName = result.text();
        }, error => console.error(error));
        this.mHttp.get(this.mBaseUrl + 'api/Job/Jobs').subscribe(result => {
            this.jobs = result.json() as Job[];
        }, error => console.error(error));
        setTimeout(() => { this.refreshPage() }, 2000);
    }
    
    ngOnInit() {        
        this.mHttp.get(this.mBaseUrl + 'api/Job/GetUserName').subscribe(result => {
            this.userName = result.text();
        }, error => console.error(error));
            this.refreshPage();        
    }
    public deleteJob(job:Job) {
        var xHttp = new XMLHttpRequest();
        xHttp.open("DELETE", this.mBaseUrl + "api/Job/DeleteJob?name=" + job.name, true);
        xHttp.send();
    }
}

interface Job {
    name: string;
    percent: number;
    message: string;
    date: Date;
    endDate: Date;
    url: string
}