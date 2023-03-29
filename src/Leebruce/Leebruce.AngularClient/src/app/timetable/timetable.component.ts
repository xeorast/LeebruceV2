import { Component, NgZone, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotAuthenticatedError } from '../api/authentication/authentication.service';
import { LessonModel, TimetableClientService, TimetableDayModel } from '../api/timetable-client/timetable-client.service';
import { TimetableViewModel } from './timetable.view-model';

@Component( {
  selector: 'app-timetable',
  templateUrl: './timetable.component.html',
  styleUrls: ['./timetable.component.scss']
} )
export class TimetableComponent implements OnInit {

  constructor(
    private router: Router,
    private timetableService: TimetableClientService ) {
    let now = new Date( Date.now() )
    this.model = <TimetableViewModel>{
      weekDays: TimetableComponent.generateWeekContaining( now ),
      currentDate: now
    }
  }

  public model: TimetableViewModel
  public currentFetch$?: Subscription

  ngOnInit(): void {
    this.today()
  }

  load( date: Date ) {
    let newModel = <TimetableViewModel>{
      weekDays: TimetableComponent.generateWeekContaining( date ),
      currentDate: date
    }
    this.model = newModel
    this.currentFetch$?.unsubscribe()
    this.currentFetch$ = this.timetableService.getTimetableForDate( date )
      .subscribe( {
        next: res => {
          newModel.timetableDays = res
          this.select( date )
        },
        error: async error => {
          if ( error instanceof NotAuthenticatedError ) {
            let currentUrl = this.router.url
            await this.router.navigate( ['/login'], { queryParams: { redirect: currentUrl } } );
          }
        }
      } )
  }

  select( newDay: Date ) {
    if ( !this.model.timetableDays ) {
      return false
    }

    for ( const day of this.model.timetableDays ) {
      if ( TimetableComponent.dateEquals( day.date, newDay ) ) {
        this.model.current = day
        this.model.currentDate = day.date
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
    let date = new Date( this.model.currentDate )
    date.setDate( date.getDate() - 7 )
    this.load( date )
  }
  next() {
    let date = new Date( this.model.currentDate )
    date.setDate( date.getDate() + 7 )
    this.load( date )
  }

  getLessonState( lesson: LessonModel ): 'completed' | 'ongoing' | 'upcomming' {
    let now = new Date( Date.now() );
    now.setSeconds( 0 )
    now.setMilliseconds( 0 )
    let shownDay = this.model.currentDate;
    let start = new Date( shownDay.getFullYear(), shownDay.getMonth(), shownDay.getDate(), lesson.time.start.hours, lesson.time.start.minutes )
    let end = new Date( shownDay.getFullYear(), shownDay.getMonth(), shownDay.getDate(), lesson.time.end.hours, lesson.time.end.minutes )

    if ( end.valueOf() < now.valueOf() ) {
      return 'completed'
    }
    if ( start.valueOf() < now.valueOf() ) {
      return 'ongoing'
    }
    return 'upcomming'
  }

  getDatePickClass( day: Date ) {
    var ret = TimetableComponent.dateEquals( day, new Date( Date.now() ) ) ? "btn-primary" : "btn-secondary";
    if ( TimetableComponent.dateEquals( day, this.model.currentDate ) ) {
      ret += " selected";
    }
    return ret;
  }

  static generateWeekContaining( day: Date ) {
    let date = new Date( day )
    date.setDate( date.getDate() - date.getDay() + 1 )
    let week: Date[] = []
    for ( let i = 0; i < 7; i++ ) {
      week.push( new Date( date ) )
      date.setDate( date.getDate() + 1 )
    }
    return week
  }

  static dateEquals( date1: Date, date2: Date ) {
    return date1.getUTCFullYear() == date2.getUTCFullYear()
      && date1.getUTCMonth() == date2.getUTCMonth()
      && date1.getUTCDate() == date2.getUTCDate()
  }

}
