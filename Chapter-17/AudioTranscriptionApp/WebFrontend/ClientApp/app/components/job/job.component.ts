import { Component, Inject, OnInit  } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'job',
    templateUrl: './job.component.html'
})
export class JobComponent implements OnInit{
    public jobs: Job[];
    private mHttp: Http;
    private mUrl: string;
    private mTimer: any;
    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.mHttp = http;
        this.mUrl = baseUrl;
        this.mTimer = setTimeout(() => { this.refreshPage() }, 0);
    }
    public refreshPage() {
        this.mHttp.get(this.mUrl + 'api/Job/Jobs').subscribe(result => {
            this.jobs = result.json() as Job[];
        }, error => console.error(error));
        this.mTimer = setTimeout(() => { this.refreshPage() }, 2000);
    }
    ngOnInit() {
        this.refreshPage();
    }
}

interface Job {
    name: string;
    percent: number;
    message: string;
    date: Date;
    url: string
}