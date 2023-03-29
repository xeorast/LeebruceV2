import { TimetableDayModel } from "../api/timetable-client/timetable-client.service"

export interface TimetableViewModel {
    weekDays: Date[]
    timetableDays?: TimetableDayModel[]
    current?: TimetableDayModel
    currentDate: Date
}
