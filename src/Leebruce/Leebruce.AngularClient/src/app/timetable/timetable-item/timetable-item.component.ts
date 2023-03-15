import { Component, HostBinding, Input, OnInit } from '@angular/core';
import { LessonModel } from 'src/app/api/timetable-client/timetable-client.service';

@Component( {
  selector: 'app-timetable-item',
  templateUrl: './timetable-item.component.html',
  styleUrls: ['./timetable-item.component.scss']
} )
export class TimetableItemComponent implements OnInit {

  @Input()
  lessonModel?: LessonModel
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
    }

    this.classForColor = `tile-${this.getRandomItem( TimetableItemComponent.tileNumbers )}`

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

}
