package info.unterrainer.nexus.restserver;

import java.nio.file.Path;

import lombok.AllArgsConstructor;
import lombok.Value;
import lombok.experimental.Accessors;

@AllArgsConstructor
@Value
@Accessors(fluent = true)
public class KeystoreConfig {

	private Path path;
	private String password;
}
