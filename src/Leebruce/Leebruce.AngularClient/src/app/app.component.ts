import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { ErrorNotifierService } from './api/error-notifier/error-notifier.service';

@Component( {
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
} )
export class AppComponent implements OnInit, OnDestroy {
  title = 'Leebruce.AngularClient';
  error?: string
  error$?: Subscription

  constructor( public errorService: ErrorNotifierService ) {
  }

  ngOnInit(): void {
    this.error$ = this.errorService.errors.subscribe( error => this.error = error.message )
  }

  ngOnDestroy(): void {
    this.error$?.unsubscribe()
  }

}
