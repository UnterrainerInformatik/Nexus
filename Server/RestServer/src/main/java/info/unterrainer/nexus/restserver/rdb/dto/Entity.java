package info.unterrainer.nexus.restserver.rdb.dto;

import java.time.LocalDateTime;

import javax.persistence.Convert;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.MappedSuperclass;

import lombok.Data;
import lombok.experimental.Accessors;

@Data
@Accessors(fluent = true)
@MappedSuperclass
public class Entity {
	@Id
	@GeneratedValue(strategy = GenerationType.IDENTITY)
	private Long id;
	private Long createdBy;
	@Convert(converter = info.unterrainer.nexus.restserver.rdb.converter.LocalDateTimeConverter.class)
	private LocalDateTime createdOn;
	private Long editedBy;
	@Convert(converter = info.unterrainer.nexus.restserver.rdb.converter.LocalDateTimeConverter.class)
	private LocalDateTime editedOn;
}
