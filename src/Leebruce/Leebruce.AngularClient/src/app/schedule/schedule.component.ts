import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotAuthenticatedError } from '../api/authentication/authentication.service';
import { ScheduleClientService, ScheduleDayModel, ScheduleEventModel } from '../api/schedule-client/schedule-client.service';
import { ScheduleViewModel } from './schedule.view-model';

@Component( {
  selector: 'app-schedule',
  templateUrl: './schedule.component.html',
  styleUrls: [
    './schedule.component.scss',
    './_calendar-page.scss',
    './_event-bubbles.scss'
  ]
} )
export class ScheduleComponent implements OnInit {

  constructor(
    private router: Router,
    private scheduleService: ScheduleClientService ) {
    let now = new Date( Date.now() )
    this.model = <ScheduleViewModel>{
      today: now,
      shownMonth: new Date( now.getFullYear(), now.getMonth(), 1 ),
      shownPageDays: ScheduleComponent.generatePage( now.getMonth(), now.getFullYear() ),
      selectedDate: now,
      daysMap: {},
      complete: false
    }
  }

  public model: ScheduleViewModel
  public currentMainFetch$?: Subscription
  public currentPrevFetch$?: Subscription
  public currentNextFetch$?: Subscription

  ngOnInit(): void {
    this.load( new Date( Date.now() ) )
  }

  load( date: Date ) {
    let newModel = <ScheduleViewModel>{
      today: new Date( Date.now() ),
      shownMonth: new Date( date.getFullYear(), date.getMonth(), 1 ),
      shownPageDays: ScheduleComponent.generatePage( date.getMonth(), date.getFullYear() ),
      selectedDate: date,
      daysMap: {},
      complete: false
    }
    this.model = newModel
    let state = { fetchesRemaining: 3 }

    this.currentMainFetch$?.unsubscribe()
    this.currentPrevFetch$?.unsubscribe()
    this.currentNextFetch$?.unsubscribe()

    this.currentMainFetch$ = this.fetchMonthAddToModel( date, newModel, state )

    let prevMonth = new Date( date )
    ScheduleComponent.setMonthSafe( prevMonth, prevMonth.getMonth() - 1 )
    this.currentPrevFetch$ = this.fetchMonthAddToModel( prevMonth, newModel, state )

    let nextMonth = new Date( date )
    ScheduleComponent.setMonthSafe( nextMonth, nextMonth.getMonth() + 1 )
    this.currentNextFetch$ = this.fetchMonthAddToModel( nextMonth, newModel, state )
  }

  fetchMonthAddToModel( date: Date, model: ScheduleViewModel, state: { fetchesRemaining: number } ) {
    return this.scheduleService.getScheduleForDate( date ).subscribe( {
      next: res => ScheduleComponent.setResult( model, res, state ),
      error: async error => await this.handleError( error )
    } )
  }

  async handleError( error: Error ) {
    if ( error instanceof NotAuthenticatedError ) {
      let currentUrl = this.router.url
      await this.router.navigate( ['/login'], { queryParams: { redirect: currentUrl } } );
    }
  }

  static setResult( model: ScheduleViewModel, res: ScheduleDayModel[], state: { fetchesRemaining: number } ) {
    let map = ScheduleComponent.sheduleToDictionary( res )
    Object.assign( model.daysMap, map )

    state.fetchesRemaining -= 1
    if ( state.fetchesRemaining == 0 ) {
      model.complete = true
    }
  }

  select( model: ScheduleViewModel, day: Date ) {
    day = new Date( day.getFullYear(), day.getMonth(), day.getDate() )
    model.selectedDate = day
  }

  goToNow() {
    let now = new Date( Date.now() )
    this.load( now )
  }

  previous() {
    let prevMonth = new Date( this.model.shownMonth )
    prevMonth.setDate( this.model.selectedDate.getDate() )
    ScheduleComponent.setMonthSafe( prevMonth, prevMonth.getMonth() - 1 )
    this.load( prevMonth )
  }

  next() {
    let nextMonth = new Date( this.model.shownMonth )
    nextMonth.setDate( this.model.selectedDate.getDate() )
    ScheduleComponent.setMonthSafe( nextMonth, nextMonth.getMonth() + 1 )

    this.load( nextMonth )
  }

  static sheduleToDictionary( shedule: ScheduleDayModel[] ) {
    let daysMap: { [dateValue: number]: ScheduleDayModel } = {};
    for ( const sheduleDay of shedule ) {
      let day = new Date( sheduleDay.day.getFullYear(), sheduleDay.day.getMonth(), sheduleDay.day.getDate() )
      daysMap[day.valueOf()] = sheduleDay
    }
    return daysMap
  }

  static generatePage( month: number, year: number ) {
    var date = new Date( year, month, 1 );
    var days = [];

    // move back to the begining of the week
    while ( date.getDay() != 1 ) {
      date.setDate( date.getDate() - 1 )
    }

    // add to the end of month
    let edgeMonth = month == 11 ? 0 : month + 1
    while ( date.getMonth() != edgeMonth ) {
      days.push( new Date( date ) );
      date.setDate( date.getDate() + 1 );
    }

    // add to the end of week
    while ( date.getDay() != 1 ) {
      days.push( new Date( date ) );
      date.setDate( date.getDate() + 1 );
    }

    return days;
  }

  monthsEqual( d1: Date, d2: Date ) {
    return d1.getFullYear() == d2.getFullYear()
      && d1.getMonth() == d2.getMonth()
  }

  datesEqual( d1: Date, d2: Date ) {
    return d1.getFullYear() == d2.getFullYear()
      && d1.getMonth() == d2.getMonth()
      && d1.getDate() == d2.getDate()
  }

  getWeek( date: Date ) {
    let idx = this.model.shownPageDays.findIndex( d =>
      date.getDate() == d.getDate()
      && date.getMonth() == d.getMonth()
      && date.getFullYear() == d.getFullYear() )

    return Math.trunc( idx / 7 )
  }

  getEventClass( event: ScheduleEventModel ) {
    if ( event.absenceData ) {
      return 'absence'
    }
    if ( event.cancellationData ) {
      return 'cancellation'
    }
    if ( event.classAbsenceData ) {
      return 'class-absence'
    }
    if ( event.freeDayData ) {
      return 'free-day'
    }
    if ( event.substitutionData ) {
      return 'substitution'
    }
    if ( event.testEtcData ) {
      return 'test'
    }
    return 'unknown-event'
  }

  static setMonthSafe( date: Date, month: number ) {
    date.setMonth( month )
    // prevent month overflow (e.g. selected 30 mar and we're moving to feb)
    if ( date.getMonth() == month + 1 ) {
      date.setDate( 0 )
    }
  }

}
