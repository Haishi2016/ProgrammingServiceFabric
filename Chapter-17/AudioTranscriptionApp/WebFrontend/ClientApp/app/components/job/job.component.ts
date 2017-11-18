import { Component } from '@angular/core';

@Component({
    selector: 'job',
    templateUrl: './job.component.html'
})
export class JobComponent {
    public currentCount = 0;

    public incrementCounter() {
        this.currentCount++;
    }
}
