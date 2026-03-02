-- 1) znajdź osierocone SizeId
SELECT vc.Id, vc.SizeId
FROM VehicleConfiguration vc
         LEFT JOIN VehicleCabSizes cs ON cs.Id = vc.SizeId
WHERE vc.SizeId IS NOT NULL AND cs.Id IS NULL;

select * from VehicleConfiguration
select * from VehicleCabSizes