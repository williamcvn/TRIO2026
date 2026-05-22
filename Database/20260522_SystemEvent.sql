select
	Id
	,se.Timestamp
	,se.TimestampLocal
	,se.ErrorId 
	,se."Level"
	,se.Category 
	,se."Source"
	,se.Message 
	,se.Detail 
	,se.UserName
	,se.UserId 
	,se.AppVersion 
	,se.MachineName 
	,se.Tags 
	,se.SessionId 
	,se.InnerException 
	,se.StackTrace 
	,se.ExceptionType 
	,se.EventCode 
	,se.CorrelationId 
FROM SystemEvent se WHERE se.TimestampLocal > '2026-05-20 00:00:00.000' 
ORDER BY
--	ErrorId ASC,
	se.TimestampLocal DESC;

SELECT * FROM user;