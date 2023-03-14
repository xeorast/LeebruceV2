import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotAuthenticatedError } from '../api/authentication/authentication.service';
import { ScheduleClientService, ScheduleDayModel, ScheduleEventModel } from '../api/schedule-client/schedule-client.service';

@Component( {
  selector: 'app-schedule',
  templateUrl: './schedule.component.html',
  styleUrls: ['./schedule.component.scss']
} )
export class ScheduleComponent implements OnInit {

  constructor(
    private router: Router,
    private scheduleService: ScheduleClientService ) { }

  public calendarPage: Date[] = []
  public currentMonth?: number
  public currentYear?: number
  public currentMonthName: string = ''
  public today?: Date
  public daysMap?: { [dateValue: number]: ScheduleDayModel | undefined };

  public selectedDay?: ScheduleDayModel

  ngOnInit(): void {
    this.initCurrentMonth()
    this.load()
  }

  load() {
    this.scheduleService.getSchedule().subscribe( {
      next: res => this.setResult( res ),
      error: async error => {
        if ( error instanceof NotAuthenticatedError ) {
          let currentUrl = this.router.url
          await this.router.navigate( ['/login'], { queryParams: { redirect: currentUrl } } );
        }
      }
    } )
  }

  initCurrentMonth() {
    let now = new Date( Date.now() )
    this.calendarPage = this.generatePage( now.getMonth(), now.getFullYear() )
    this.currentMonth = now.getMonth()
    this.currentYear = now.getFullYear()
    this.currentMonthName = now.toLocaleDateString( 'en-US', { month: 'long' } )
    this.today = now
  }

  setResult( res: ScheduleDayModel[] ) {
    let now = new Date( Date.now() )
    // now.setDate( now.getDate() + -2 )
    this.calendarPage = this.generatePage( now.getMonth(), now.getFullYear() )
    this.daysMap = this.sheduleToDictionary( res )
    this.currentMonth = now.getMonth()
    this.currentYear = now.getFullYear()
    this.currentMonthName = now.toLocaleDateString( 'en-US', { month: 'long' } )
    this.today = now

    this.select( now )
  }

  select( day: Date ) {
    day = new Date( day.getFullYear(), day.getMonth(), day.getDate() )
    this.selectedDay = this.daysMap?.[day.valueOf()]
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
    while ( date.getMonth() != month + 1 ) {
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
