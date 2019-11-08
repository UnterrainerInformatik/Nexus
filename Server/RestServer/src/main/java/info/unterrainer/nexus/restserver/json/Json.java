package info.unterrainer.nexus.restserver.json;

import java.time.LocalDateTime;

import lombok.Data;
import lombok.experimental.Accessors;

@Data
@Accessors(fluent = true)
public class Json {

	private Long id;
	private Long createdBy;
	private Long modifiedBy;
	private LocalDateTime createdOn;
	private LocalDateTime modifiedOn;
}
