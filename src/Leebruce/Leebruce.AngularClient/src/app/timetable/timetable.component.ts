import { Component, NgZone, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotAuthenticatedError } from '../api/authentication/authentication.service';
import { TimetableClientService, TimetableDayModel } from '../api/timetable-client/timetable-client.service';

@Component( {
  selector: 'app-timetable',
  templateUrl: './timetable.component.html',
  styleUrls: ['./timetable.component.scss']
} )
export class TimetableComponent implements OnInit {

  constructor(
    private router: Router,
    private timetableService: TimetableClientService ) { }

  public timetableDays?: TimetableDayModel[]
  public current?: TimetableDayModel

  ngOnInit(): void {
    this.today()
  }

  load( date: Date ) {
    this.timetableService.getTimetableForDate( date ).subscribe( {
      next: res => this.setResult( res, date ),
      error: async error => {
        if ( error instanceof NotAuthenticatedError ) {
          let currentUrl = this.router.url
          await this.router.navigate( ['/login'], { queryParams: { redirect: currentUrl } } );
        }
      }
    } )
  }

  setResult( res: TimetableDayModel[], date: Date ) {
    this.timetableDays = res
    this.select( date )
  }

  select( newDay: Date ) {
    if ( !this.timetableDays ) {
      return false
    }

    for ( const day of this.timetableDays ) {
      if ( this.dateEquals( day.date, newDay ) ) {
        this.current = day
        return true
      }
    }
    return false
  }

  today() {
    let now = new Date( Date.now() )
    if ( !this.select( now ) ) {
      this.load( now )
    }
  }
  previous() {
    if ( !this.current ) {
      return
    }

    let date = new Date( this.current.date )
    date.setDate( date.getDate() - 7 )
    this.load( date )
  }
  next() {
    if ( !this.current ) {
      return
    }

    let date = new Date( this.current.date )
    date.setDate( date.getDate() + 7 )
    this.load( date )
  }

  getDatePickClass( day: TimetableDayModel ) {
    var ret = this.dateEquals( day.date, new Date( Date.now() ) ) ? "btn-primary" : "btn-secondary";
    if ( day.date == this.current?.date ) {
      ret += " selected";
    }
    return ret;
  }

  dateEquals( date1: Date, date2: Date ) {
    return date1.getUTCFullYear() == date2.getUTCFullYear()
      && date1.getUTCMonth() == date2.getUTCMonth()
      && date1.getUTCDate() == date2.getUTCDate()
  }

}
