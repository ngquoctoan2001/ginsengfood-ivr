SELECT json_build_object(
'tasks',(SELECT count(*) FROM ivr_confirmation_tasks),
'attempts',(SELECT count(*) FROM ivr_call_attempts),
'counted_customer_attempts',(SELECT count(*) FROM ivr_call_attempts WHERE is_counted_customer_attempt),
'technical_attempts',(SELECT count(*) FROM ivr_call_attempts WHERE technical_exception_type IS NOT NULL),
'results',(SELECT count(*) FROM ivr_call_results),
'final_results',(SELECT count(*) FROM ivr_call_results WHERE is_final_for_ivr),
'callbacks',(SELECT count(*) FROM ivr_result_callbacks),
'miscounted_technical_attempts',(SELECT count(*) FROM ivr_call_attempts WHERE technical_exception_type IS NOT NULL AND is_counted_customer_attempt),
'miscounted_noncustomer_results',(SELECT count(*) FROM ivr_call_results WHERE result_type IN ('IVR_TECHNICAL_EXCEPTION','IVR_CAPACITY_EXCEPTION','IVR_CONFIRMATION_WINDOW_EXPIRED') AND is_counted_customer_attempt),
'duplicate_finals',(SELECT count(*) FROM (SELECT task_id FROM ivr_call_results WHERE is_final_for_ivr GROUP BY task_id HAVING count(*)>1) x),
'duplicate_callbacks',(SELECT count(*) FROM (SELECT task_id FROM ivr_result_callbacks GROUP BY task_id HAVING count(*)>1) x),
'callbacks_for_nonfinal_results',(SELECT count(*) FROM ivr_result_callbacks c JOIN ivr_call_results r ON r.ivr_call_result_id=c.ivr_call_result_id WHERE NOT r.is_final_for_ivr));
