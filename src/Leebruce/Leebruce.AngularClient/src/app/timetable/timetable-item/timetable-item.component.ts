import { Component, HostBinding, Input, OnInit } from '@angular/core';
import { LessonModel } from 'src/app/api/timetable-client/timetable-client.service';

@Component( {
  selector: 'app-timetable-item',
  templateUrl: './timetable-item.component.html',
  styleUrls: [
    './timetable-item.component.scss',
    './_marker.scss'
  ]
} )
export class TimetableItemComponent implements OnInit {

  @Input()
  lessonModel?: LessonModel
  @Input()
  subjectSuggestion?: string
  @Input()
  state!: 'completed' | 'ongoing' | 'upcomming'

  teacherName?: string
  time?: string

  @HostBinding( 'class.completed' ) isCompleted = false
  @HostBinding( 'class.ongoing' ) isOngoing = false
  @HostBinding( 'class.upcomming' ) isUpcomming = false

  classFromCancellation?: string
  classForColor?: string

  constructor() { }

  ngOnInit(): void {
    if ( this.lessonModel ) {
      this.teacherName = `${this.lessonModel.teacherName} ${this.lessonModel.teacherSurname}`
      let start = this.lessonModel.time.start
      let end = this.lessonModel.time.end
      this.time = `${start.hours}:${start.minutes} - ${end.hours}:${end.minutes}`

      this.classFromCancellation = this.lessonModel.isCancelled || this.lessonModel.classAbsence ? 'cancelled' : undefined;
      this.classForColor = `tile-${TimetableItemComponent.getColorClass( this.lessonModel?.subject )}`
    }
    if ( this.subjectSuggestion != undefined ) {
      this.classForColor ??= `tile-${TimetableItemComponent.getColorClass( this.subjectSuggestion )}`
    }
    this.classForColor ??= `tile-${this.getRandomItem( TimetableItemComponent.tileNumbers )}`

    this.isCompleted = this.state == 'completed'
    this.isOngoing = this.state == 'ongoing'
    this.isUpcomming = this.state == 'upcomming'
  }

  static tileNumbers = [0, 1, 2, 3, 4, 6, 8, 10, 14, 16, 18, 20, 22, 24, 26, 28, 32]
  getRandomInt( min: number, max: number ) {
    min = Math.ceil( min );
    max = Math.floor( max );
    return Math.floor( Math.random() * ( max - min ) + min );
  }

  getRandomItem( arr: any[] ) {
    return arr[this.getRandomInt( 0, arr.length )]
  }

  static getKnownClass( subject: string ) {
    switch ( subject ) {
      case "Język angielski":
        return 12;
      case "Fizyka":
        return 7;
      case "Religia":
        return 0;
      case "Historia":
        return 18;
      case "Wychowanie fizyczne":
        return 30;
      default:
        return undefined
    }
  }

  static hash( str: string ) {
    let hash = 0
    let chr
    for ( let i = 0; i < str.length; i++ ) {
      chr = str.charCodeAt( i );
      hash = ( ( hash << 5 ) - hash ) + chr;
      hash |= 0; // Convert to 32bit integer
    }
    return hash;
  }

  static getColorClass( subject: string ) {
    let known = TimetableItemComponent.getKnownClass( subject )
    if ( known != undefined ) {
      return known
    }

    let hash = Math.abs( TimetableItemComponent.hash( subject ) )
    let hash2 = Math.floor( Math.sqrt( hash ) )
    let idx = Math.abs( hash2 ) % TimetableItemComponent.tileNumbers.length
    return TimetableItemComponent.tileNumbers[idx]
  }

}
