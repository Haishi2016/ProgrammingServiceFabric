import { Component, Inject  } from '@angular/core';
import { Http } from '@angular/http';

@Component({
    selector: 'job',
    templateUrl: './job.component.html'
})
export class JobComponent {
    public jobs: Job[];

    constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
        http.get(baseUrl + 'api/Job/Jobs').subscribe(result => {
            this.jobs = result.json() as Job[];
        }, error => console.error(error));
    }
}

interface Job {
    name: string;
    percent: number;
    message: string;
    url: string
}