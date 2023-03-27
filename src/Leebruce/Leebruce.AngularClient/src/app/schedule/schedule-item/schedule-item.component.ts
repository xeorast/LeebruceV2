import { Component, Input, OnInit } from '@angular/core';
import { ScheduleEventModel } from 'src/app/api/schedule-client/schedule-client.service';

@Component( {
  selector: 'app-schedule-item',
  templateUrl: './schedule-item.component.html',
  styleUrls: ['./schedule-item.component.scss']
} )
export class ScheduleItemComponent implements OnInit {

  @Input()
  public eventModel?: ScheduleEventModel

  public timeLine1: string = ''
  public timeLine2: string = ''
  public heading: string = ''
  public detail: string = ''

  public enlargeTimeLine1: boolean = false
  public reduceTimeLine2: boolean = false

  constructor() { }

  ngOnInit(): void {
    if ( this.eventModel?.absenceData ) {
      this.timeLine1 = this.eventModel.absenceData.time?.start.substring( 0, 5 ) ?? 'All'
      this.timeLine2 = this.eventModel.absenceData.time?.end.substring( 0, 5 ) ?? 'Day'
      this.heading = this.eventModel.absenceData.who
      this.detail = 'Absent'
    }
    else if ( this.eventModel?.cancellationData ) {
      this.timeLine1 = this.eventModel.cancellationData.lessonNo.toString()
      this.timeLine2 = 'Lesson'
      this.enlargeTimeLine1 = true
      this.reduceTimeLine2 = true
      this.heading = this.eventModel.cancellationData.subject
      this.detail = 'Canceled'
    }
    else if ( this.eventModel?.classAbsenceData ) {
      //todo: normal when
      let when = this.eventModel.classAbsenceData.when.split( ': ' )
      this.timeLine1 = when[0]
      this.timeLine2 = when[1]
      this.heading = this.eventModel.classAbsenceData.class
      this.detail = 'Absent class'
    }
    else if ( this.eventModel?.freeDayData ) {
      this.timeLine1 = 'All'
      this.timeLine2 = "Day"
      this.heading = this.eventModel.freeDayData.what
      this.detail = this.eventModel.freeDayData.who
    }
    else if ( this.eventModel?.substitutionData ) {
      this.timeLine1 = this.eventModel.substitutionData.lessonNo.toString()
      this.timeLine2 = 'Lesson'
      this.enlargeTimeLine1 = true
      this.reduceTimeLine2 = true
      this.heading = this.eventModel.substitutionData.subject
      this.detail = this.eventModel.substitutionData.who
    }
    else if ( this.eventModel?.testEtcData ) {
      if ( this.eventModel.testEtcData.lessonNo ) {
        this.timeLine1 = this.eventModel.testEtcData.lessonNo?.toString()
        this.timeLine2 = 'Lesson'
        this.reduceTimeLine2 = true
      }
      this.heading = ( this.eventModel.testEtcData.subject && this.eventModel.testEtcData.what )
        ? `${this.eventModel.testEtcData.subject} - ${this.eventModel.testEtcData.what}`
        : this.eventModel.testEtcData.subject ?? this.eventModel.testEtcData.what ?? 'Something happening'
      this.detail = this.eventModel.testEtcData.description ?? ''
    }
    else if ( this.eventModel?.unrecognizedData ) {
      this.detail = this.eventModel?.unrecognizedData.value ?? 'Error parsing event'
    }


  }

}
