package info.unterrainer.nexus.restserver.rdb.converter;

import java.sql.Timestamp;
import java.time.LocalDateTime;

import javax.persistence.AttributeConverter;
import javax.persistence.Converter;

@Converter(autoApply = true)
public class LocalDateTimeConverter implements AttributeConverter<LocalDateTime, Timestamp> {

	@Override
	public Timestamp convertToDatabaseColumn(LocalDateTime attribute) {
		return attribute == null ? null : Timestamp.valueOf(attribute);
	}

	@Override
	public LocalDateTime convertToEntityAttribute(Timestamp dbData) {
		return dbData == null ? null : dbData.toLocalDateTime();
	}
}
