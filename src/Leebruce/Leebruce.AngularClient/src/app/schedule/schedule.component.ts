import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotAuthenticatedError } from '../api/authentication/authentication.service';
import { ScheduleClientService, ScheduleDayModel, ScheduleEventModel } from '../api/schedule-client/schedule-client.service';

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
    private scheduleService: ScheduleClientService ) { }

  public calendarPage: Date[] = []
  public currentMonth?: Date
  public today?: Date
  public daysMap?: { [dateValue: number]: ScheduleDayModel | undefined };
  public additionalDaysMap?: { [dateValue: number]: ScheduleDayModel | undefined };
  private resetAdditionalDaysMap?: boolean

  public selectedDay?: ScheduleDayModel
  public selectedDate?: Date

  private loadedMonth?: Date

  ngOnInit(): void {
    this.initCurrentMonth()
    this.load( new Date( Date.now() ) )
  }

  load( date: Date ) {
    this.scheduleService.getScheduleForDate( date ).subscribe( {
      next: res => this.setResult( res, date ),
      error: async error => await this.handleError( error )
    } )

    this.resetAdditionalDaysMap = true
    let prevDate = new Date( date )
    prevDate.setMonth( prevDate.getMonth() - 1 )
    this.scheduleService.getScheduleForDate( prevDate ).subscribe( {
      next: res => this.setAdditionalResult( res, prevDate ),
      error: async error => await this.handleError( error )
    } )

    let nextDate = new Date( date )
    nextDate.setMonth( nextDate.getMonth() + 1 )
    this.scheduleService.getScheduleForDate( nextDate ).subscribe( {
      next: res => this.setAdditionalResult( res, nextDate ),
      error: async error => await this.handleError( error )
    } )
  }

  async handleError( error: Error ) {
    if ( error instanceof NotAuthenticatedError ) {
      let currentUrl = this.router.url
      await this.router.navigate( ['/login'], { queryParams: { redirect: currentUrl } } );
    }
  }

  initCurrentMonth() {
    let now = new Date( Date.now() )
    this.today = now
    this.initPage( now )
  }

  setResult( res: ScheduleDayModel[], date: Date ) {
    this.daysMap = this.sheduleToDictionary( res )
    this.loadedMonth = new Date( date.getFullYear(), date.getMonth(), 1 )
    this.initPage( date )
    this.select( date )
  }

  setAdditionalResult( res: ScheduleDayModel[], date: Date ) {
    let dict = this.sheduleToDictionary( res )
    if ( this.resetAdditionalDaysMap ) {
      this.resetAdditionalDaysMap = false
      this.additionalDaysMap = {}
    }

    this.additionalDaysMap ??= {}
    for ( const key in dict ) {
      this.additionalDaysMap[key] = dict[key]
    }
  }

  initPage( date: Date ) {
    this.calendarPage = this.generatePage( date.getMonth(), date.getFullYear() )
    this.currentMonth = new Date( date.getFullYear(), date.getMonth(), 1 )
  }

  select( day: Date ) {
    day = new Date( day.getFullYear(), day.getMonth(), day.getDate() )
    this.selectedDay = this.daysMap?.[day.valueOf()] ?? this.additionalDaysMap?.[day.valueOf()]
    this.selectedDate = day
  }

  goToNow() {
    let now = new Date( Date.now() )
    if ( this.loadedMonth && this.monthsEqual( this.loadedMonth, now ) ) {
      this.select( now )
    }
    else {
      this.load( now )
    }
  }

  previous() {
    if ( !this.currentMonth ) {
      return
    }

    let date = new Date( this.currentMonth )
    date.setMonth( date.getMonth() - 1 )
    if ( this.selectedDate ) {
      date.setDate( this.selectedDate.getDate() )
      console.log( date )
    }
    this.load( date )
  }

  next() {
    if ( !this.currentMonth ) {
      return
    }

    let date = new Date( this.currentMonth )
    date.setMonth( date.getMonth() + 1 )
    if ( this.selectedDate ) {
      date.setDate( this.selectedDate.getDate() )
    }
    this.load( date )
  }

  sheduleToDictionary( shedule: ScheduleDayModel[] ) {
    let daysMap: { [dateValue: number]: ScheduleDayModel } = {};
    for ( const sheduleDay of shedule ) {
      let day = new Date( sheduleDay.day.getFullYear(), sheduleDay.day.getMonth(), sheduleDay.day.getDate() )
      daysMap[day.valueOf()] = sheduleDay
    }
    return daysMap
  }

  generatePage( month: number, year: number ) {
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
    let idx = this.calendarPage.findIndex( d =>
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
    if ( event.substitutionData ) {
      return 'substitution'
    }
    if ( event.testEtcData ) {
      return 'test'
    }
    return 'unknown-event'
  }

}
